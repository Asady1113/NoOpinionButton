# Deployment Scripts

This directory contains deployment scripts for the NoOpinionButtonWeb application.

## Scripts Overview

### 1. `build.sh`
Builds the Nuxt.js application for static site generation.

**Usage:**
```bash
./scripts/build.sh
```

**What it does:**
- Installs dependencies if needed
- Cleans previous builds
- Runs `npm run generate` to create static files
- Validates build output
- Provides build summary

### 2. `deploy-infrastructure.sh`
Deploys the AWS CDK infrastructure stack.

**Prerequisites:**
- AWS CLI installed and configured
- AWS CDK installed (script will install if missing)
- Valid AWS credentials with appropriate permissions

**Usage:**
```bash
./scripts/deploy-infrastructure.sh
```

**What it does:**
- Validates AWS credentials and CDK setup
- Installs CDK dependencies
- Bootstraps CDK if needed
- Synthesizes and deploys the CDK stack
- Shows stack outputs

### 3. `deploy.sh`
Uploads built static files to S3 and invalidates CloudFront cache.

**Prerequisites:**
- Application must be built first (`./scripts/build.sh`)
- Infrastructure must be deployed first (`./scripts/deploy-infrastructure.sh`)
- AWS CLI configured with appropriate permissions

**Usage:**
```bash
./scripts/deploy.sh
```

**Alternative with environment variable:**
```bash
S3_BUCKET_NAME=your-bucket-name ./scripts/deploy.sh
```

**What it does:**
- Validates build output exists
- Determines S3 bucket name from CDK outputs or environment
- Uploads files to S3 with appropriate cache headers
- Invalidates CloudFront cache
- Provides deployment summary

## Complete Deployment Workflow

1. **Deploy Infrastructure:**
   ```bash
   ./scripts/deploy-infrastructure.sh
   ```

2. **Build Application:**
   ```bash
   ./scripts/build.sh
   ```

3. **Deploy Application:**
   ```bash
   ./scripts/deploy.sh
   ```

## Error Handling

All scripts include comprehensive error handling:
- Exit on any error (`set -e`)
- Validation of prerequisites
- Clear error messages with troubleshooting hints
- Verification of successful completion

## Troubleshooting

### Common Issues

1. **AWS credentials not configured:**
   ```bash
   aws configure
   ```

2. **CDK not bootstrapped:**
   ```bash
   cd infrastructure
   npx cdk bootstrap
   ```

3. **Build directory not found:**
   Run the build script first: `./scripts/build.sh`

4. **S3 bucket access denied:**
   Check AWS permissions and bucket name