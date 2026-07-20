using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.AWSSettings
{
    public sealed class AwsSettings
    {
        public const string SectionName = "AWS";

        public string AccessKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public string BucketName { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;
    }
}
