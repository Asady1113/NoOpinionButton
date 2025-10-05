import * as cdk from 'aws-cdk-lib';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as cloudfront from 'aws-cdk-lib/aws-cloudfront';
import * as origins from 'aws-cdk-lib/aws-cloudfront-origins';
import { Construct } from 'constructs';

export class NoOpinionButtonWebStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // S3 Bucket for static website hosting
    const bucket = new s3.Bucket(this, 'Bucket', {
      bucketName: 'no-opinion-button-web',
      // トップページもエラーページも index.html を返すようにしています
      websiteIndexDocument: 'index.html',
      websiteErrorDocument: 'index.html',
      // 誰でもバケット内のファイルにアクセスできるようにする
      publicReadAccess: true,
      // スタック削除時にバケットごと削除する
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: true,
    });

    // CloudFront Distribution
    const distribution = new cloudfront.Distribution(this, 'WebsiteDistribution', {
      defaultBehavior: {
        // 配信元は S3 バケットのウェブサイトドメイン
        origin: new origins.HttpOrigin(bucket.bucketWebsiteDomainName),
        // HTTPS にリダイレクト
        viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
        // JS/CSS/画像などの静的アセットは長めにキャッシュ、HTML は短めにキャッシュ
        cachePolicy: cloudfront.CachePolicy.CACHING_OPTIMIZED,
      },
      // / にアクセスしたときに index.html を返す
      defaultRootObject: 'index.html',
    });

    // Outputs
    // 結果を出力させる
    new cdk.CfnOutput(this, 'S3BucketName', {
      value: bucket.bucketName,
      description: 'Name of the S3 bucket for website hosting',
    });

    new cdk.CfnOutput(this, 'CloudFrontDistributionId', {
      value: distribution.distributionId,
      description: 'CloudFront Distribution ID',
    });

    new cdk.CfnOutput(this, 'CloudFrontDomainName', {
      value: distribution.distributionDomainName,
      description: 'CloudFront Distribution Domain Name',
    });
  }
}