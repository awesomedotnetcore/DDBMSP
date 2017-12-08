﻿using System;
using System.Threading.Tasks;
using DDBMSP.Grains.Core;
using DDBMSP.Interfaces.Grains;
using DDBMSP.Interfaces.PODs.Article.Components;
using DDBMSP.Interfaces.PODs.User;
using DDBMSP.Interfaces.PODs.User.Components;
using Orleans;

namespace DDBMSP.Grains
{
    public class User : ResourceGrain<UserState, IUserData>, IUser
    {
        public Task<Guid> AuthorNewArticle(IArticleData articleData)
        {
            var article = GrainFactory.GetGrain<IArticle>(Guid.NewGuid());
            article.PopulateFromAuthorAndData(this, articleData).Ignore();
            State.Articles.Add(article);
            return Task.FromResult(article.GetPrimaryKey());
        }
    }
}