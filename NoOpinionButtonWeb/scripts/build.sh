#!/bin/bash

# Nuxt.js Application Build Script
# This script builds the Nuxt.js application for static site generation

set -e  # Exit on any error

echo "🚀 Starting Nuxt.js application build..."

# Check if we're in the correct directory
if [ ! -f "package.json" ]; then
    echo "❌ Error: package.json not found. Please run this script from the NoOpinionButtonWeb directory."
    exit 1
fi

# Check if node_modules exists, if not run npm install
if [ ! -d "node_modules" ]; then
    echo "📦 Installing dependencies..."
    npm ci
else
    echo "📦 Dependencies already installed, skipping npm ci..."
fi

# Clean previous build
if [ -d "dist" ]; then
    echo "🧹 Cleaning previous build..."
    rm -rf dist
fi

if [ -d ".output" ]; then
    echo "🧹 Cleaning previous output..."
    rm -rf .output
fi

# Build the application for static site generation
echo "🔨 Building Nuxt.js application for static generation..."
npm run generate

# Verify build output
if [ ! -d "dist" ] && [ ! -d ".output/public" ]; then
    echo "❌ Error: Build failed - no output directory found"
    exit 1
fi

# Check if .output/public exists and copy to dist for consistency
if [ -d ".output/public" ]; then
    echo "📁 Copying .output/public to dist for consistency..."
    rm -rf dist
    cp -r .output/public dist
fi

# Verify essential files exist (check for either index.html or 200.html)
if [ ! -f "dist/index.html" ] && [ ! -f "dist/200.html" ]; then
    echo "❌ Error: No main HTML file found in build output"
    exit 1
fi

# If we have 200.html but no index.html, create index.html as a copy
if [ -f "dist/200.html" ] && [ ! -f "dist/index.html" ]; then
    echo "📁 Creating index.html from 200.html for static hosting..."
    cp dist/200.html dist/index.html
fi

echo "✅ Build completed successfully!"
echo "📁 Build output available in: dist/"

# Display build summary
echo ""
echo "📊 Build Summary:"
echo "   - Total files: $(find dist -type f | wc -l)"
echo "   - Build size: $(du -sh dist | cut -f1)"
echo ""