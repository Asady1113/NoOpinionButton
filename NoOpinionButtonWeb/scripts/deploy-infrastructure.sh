#!/bin/bash

# CDK Infrastructure Deployment Script
# This script deploys the AWS CDK stack for web deployment infrastructure

set -e  # Exit on any error

echo "🏗️  Starting CDK infrastructure deployment..."

# Check if we're in the correct directory structure
if [ ! -d "infrastructure" ]; then
    echo "❌ Error: infrastructure directory not found. Please run this script from the NoOpinionButtonWeb directory."
    exit 1
fi

# Navigate to infrastructure directory
cd infrastructure

# Check if CDK project files exist
if [ ! -f "package.json" ]; then
    echo "❌ Error: package.json not found in infrastructure directory."
    exit 1
fi

if [ ! -f "cdk.json" ]; then
    echo "❌ Error: cdk.json not found. This doesn't appear to be a CDK project."
    exit 1
fi

# Check if AWS CLI is installed and configured
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

# Install CDK dependencies if node_modules doesn't exist
if [ ! -d "node_modules" ]; then
    echo "📦 Installing CDK dependencies..."
    npm ci
else
    echo "📦 CDK dependencies already installed, skipping npm ci..."
fi

# Check if CDK is installed globally, if not install it
if ! command -v cdk &> /dev/null; then
    echo "📦 Installing AWS CDK globally..."
    npm install -g aws-cdk
fi

# Bootstrap CDK if needed (this is safe to run multiple times)
echo "🔧 Bootstrapping CDK (if needed)..."
npx cdk bootstrap

# Synthesize the stack to check for errors
echo "🔍 Synthesizing CDK stack..."
npx cdk synth

# Deploy the stack
echo "🚀 Deploying CDK stack..."
npx cdk deploy --require-approval never

# Verify deployment
if [ $? -eq 0 ]; then
    echo "✅ CDK stack deployed successfully!"
    
    # Get stack outputs
    echo ""
    echo "📋 Stack Outputs:"
    npx cdk list --long 2>/dev/null || echo "   (Use 'npx cdk list --long' to see stack details)"
    
    echo ""
    echo "💡 Next steps:"
    echo "   1. Run the build script: ./scripts/build.sh"
    echo "   2. Run the deploy script: ./scripts/deploy.sh"
else
    echo "❌ CDK deployment failed!"
    exit 1
fi

# Return to original directory
cd ..

echo ""
echo "🎉 Infrastructure deployment completed!"