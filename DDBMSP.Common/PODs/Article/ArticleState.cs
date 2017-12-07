﻿using System;
using System.Collections.Generic;
using DDBMSP.Common.Enums;
using DDBMSP.Common.PODs.Article.Components;
using DDBMSP.Common.PODs.Core;

namespace DDBMSP.Common.PODs.Article
{
    public class ArticleState : Exist, IArticleData
    {
        public DateTime CreationDate { get; set; }
        public string Title { get; set; }
        public ArticleCategory Catergory { get; set; }
        public string Abstract { get; set; }
        public List<string> Tags { get; set; }
        public Guid AuthorId { get; set; }
        public Language Language { get; set; }
        public Uri ContentTextUri { get; set; }
        public Uri ContentImageUri { get; set; }
        public Uri ContentVideoUri { get; set; }
    }
}