#!/bin/bash

# S3 Deployment Script
# This script uploads the built static files to S3 bucket

set -e  # Exit on any error

echo "📤 Starting S3 deployment..."

# Check if build output exists
if [ ! -d "dist" ]; then
    echo "❌ Error: dist directory not found. Please run the build script first: ./scripts/build.sh"
    exit 1
fi

# Verify essential files exist
if [ ! -f "dist/index.html" ]; then
    echo "❌ Error: index.html not found in dist directory. Build may have failed."
    exit 1
fi

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "❌ Error: AWS CLI is not installed. Please install AWS CLI first."
    exit 1
fi

# Check AWS credentials
if ! aws sts get-caller-identity &> /dev/null; then
    echo "❌ Error: AWS credentials not configured or invalid. Please configure AWS CLI first."
    exit 1
fi

echo "✅ AWS credentials verified"

# Get S3 bucket name from CDK outputs or environment variable
BUCKET_NAME=""

# Try to get bucket name from CDK outputs first
if [ -d "infrastructure" ]; then
    cd infrastructure
    BUCKET_NAME=$(npx cdk list --json 2>/dev/null | jq -r '.[0]' 2>/dev/null || echo "")
    if [ -n "$BUCKET_NAME" ] && [ "$BUCKET_NAME" != "null" ]; then
        # Try to get the actual bucket name from stack outputs
        STACK_NAME="$BUCKET_NAME"
        BUCKET_NAME=$(aws cloudformation describe-stacks --stack-name "$STACK_NAME" --query 'Stacks[0].Outputs[?OutputKey==`S3BucketName`].OutputValue' --output text 2>/dev/null || echo "")
    fi
    cd ..
fi

# If CDK method failed, try environment variable
if [ -z "$BUCKET_NAME" ] || [ "$BUCKET_NAME" == "None" ]; then
    BUCKET_NAME="$S3_BUCKET_NAME"
fi

# If still no bucket name, prompt user
if [ -z "$BUCKET_NAME" ]; then
    echo "🔍 S3 bucket name not found automatically."
    echo "Please provide the S3 bucket name:"
    echo "You can find it in:"
    echo "  - AWS CloudFormation console outputs"
    echo "  - Set S3_BUCKET_NAME environment variable"
    echo ""
    read -p "Enter S3 bucket name: " BUCKET_NAME
fi

if [ -z "$BUCKET_NAME" ]; then
    echo "❌ Error: S3 bucket name is required"
    exit 1
fi

echo "📁 Using S3 bucket: $BUCKET_NAME"

# Verify bucket exists and is accessible
if ! aws s3 ls "s3://$BUCKET_NAME" &> /dev/null; then
    echo "❌ Error: Cannot access S3 bucket '$BUCKET_NAME'. Please check:"
    echo "   - Bucket name is correct"
    echo "   - AWS credentials have access to the bucket"
    echo "   - Bucket exists in the current AWS region"
    exit 1
fi

echo "✅ S3 bucket access verified"

# Upload files to S3 with proper content types
echo "📤 Uploading files to S3..."

# Sync files with delete flag to remove old files
aws s3 sync ./dist/ "s3://$BUCKET_NAME/" \
    --delete \
    --cache-control "public, max-age=31536000" \
    --exclude "*.html" \
    --exclude "*.json"

# Upload HTML files with no-cache for dynamic content
aws s3 sync ./dist/ "s3://$BUCKET_NAME/" \
    --cache-control "no-cache, no-store, must-revalidate" \
    --content-type "text/html" \
    --include "*.html"

# Upload JSON files with appropriate cache control
aws s3 sync ./dist/ "s3://$BUCKET_NAME/" \
    --cache-control "public, max-age=300" \
    --content-type "application/json" \
    --include "*.json"

# Verify upload
UPLOADED_FILES=$(aws s3 ls "s3://$BUCKET_NAME/" --recursive | wc -l)
if [ "$UPLOADED_FILES" -eq 0 ]; then
    echo "❌ Error: No files found in S3 bucket after upload"
    exit 1
fi

echo "✅ Upload completed successfully!"

# Get CloudFront distribution ID for cache invalidation
echo "🔄 Attempting to invalidate CloudFront cache..."

DISTRIBUTION_ID=""
if [ -d "infrastructure" ]; then
    cd infrastructure
    STACK_NAME=$(npx cdk list --json 2>/dev/null | jq -r '.[0]' 2>/dev/null || echo "")
    if [ -n "$STACK_NAME" ] && [ "$STACK_NAME" != "null" ]; then
        DISTRIBUTION_ID=$(aws cloudformation describe-stacks --stack-name "$STACK_NAME" --query 'Stacks[0].Outputs[?OutputKey==`CloudFrontDistributionId`].OutputValue' --output text 2>/dev/null || echo "")
    fi
    cd ..
fi

if [ -n "$DISTRIBUTION_ID" ] && [ "$DISTRIBUTION_ID" != "None" ]; then
    echo "🔄 Invalidating CloudFront cache for distribution: $DISTRIBUTION_ID"
    INVALIDATION_ID=$(aws cloudfront create-invalidation \
        --distribution-id "$DISTRIBUTION_ID" \
        --paths "/*" \
        --query 'Invalidation.Id' \
        --output text)
    
    if [ -n "$INVALIDATION_ID" ]; then
        echo "✅ Cache invalidation created: $INVALIDATION_ID"
        echo "💡 Cache invalidation may take 5-15 minutes to complete"
    fi
else
    echo "⚠️  CloudFront distribution ID not found. Cache invalidation skipped."
    echo "💡 You may need to manually invalidate the CloudFront cache"
fi

echo ""
echo "📊 Deployment Summary:"
echo "   - S3 Bucket: $BUCKET_NAME"
echo "   - Files uploaded: $UPLOADED_FILES"
if [ -n "$DISTRIBUTION_ID" ]; then
    echo "   - CloudFront Distribution: $DISTRIBUTION_ID"
    DOMAIN_NAME=$(aws cloudformation describe-stacks --stack-name "$STACK_NAME" --query 'Stacks[0].Outputs[?OutputKey==`CloudFrontDomainName`].OutputValue' --output text 2>/dev/null || echo "")
    if [ -n "$DOMAIN_NAME" ] && [ "$DOMAIN_NAME" != "None" ]; then
        echo "   - Website URL: https://$DOMAIN_NAME"
    fi
fi

echo ""
echo "🎉 Deployment completed successfully!"