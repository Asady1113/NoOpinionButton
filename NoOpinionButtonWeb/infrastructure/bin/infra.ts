#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from 'aws-cdk-lib';
import { NoOpinionButtonWebStack } from '../lib/no-opinion-button-web-stack';

const app = new cdk.App();

new NoOpinionButtonWebStack(app, 'NoOpinionButtonWebStack', {
  // CDK が cdk bootstrap や cdk deploy したときに自動で環境変数に入れてくれる値
  env: {
    account: process.env.CDK_DEFAULT_ACCOUNT,
    region: process.env.CDK_DEFAULT_REGION,
  },
});