# Web Deployment Infrastructure

This CDK project sets up the infrastructure for deploying the NoOpinionButtonWeb application using AWS S3 and CloudFront.

## Prerequisites

- AWS CLI configured with appropriate credentials
- Node.js (version 18 or later)
- AWS CDK CLI installed globally: `npm install -g aws-cdk`

## Setup

1. Install dependencies:
   ```bash
   npm install
   ```

2. Bootstrap CDK (first time only):
   ```bash
   npx cdk bootstrap
   ```

3. Deploy the stack:
   ```bash
   npx cdk deploy
   ```

## Architecture

- **S3 Bucket**: Hosts static website files with public read access
- **CloudFront Distribution**: CDN for global content delivery with SPA support (404 → index.html)

## Outputs

After deployment, the stack provides:
- S3 bucket name for file uploads
- CloudFront distribution ID for cache invalidation
- CloudFront domain name for accessing the website

## Commands

- `npm run build`: Compile TypeScript to JavaScript
- `npm run watch`: Watch for changes and compile
- `npx cdk diff`: Compare deployed stack with current state
- `npx cdk synth`: Emit the synthesized CloudFormation template
- `npx cdk deploy`: Deploy this stack to your default AWS account/region
- `npx cdk destroy`: Destroy the stack