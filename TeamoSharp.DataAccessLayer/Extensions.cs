using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TeamoSharp.DataAccessLayer
{
    public static class Extensions
    {
        public static Entities.ClientMessage AsEntityType(this Models.ClientMessage modelMessage)
        {
            return new Entities.ClientMessage
            {
                MessageId = modelMessage.MessageId,
                ChannelId = modelMessage.ChannelId,
                ServerId = modelMessage.ServerId
            };
        }

        public static Models.ClientMessage AsModelType(this Entities.ClientMessage entityMessage)
        {
            return new Models.ClientMessage
            {
                MessageId = entityMessage.MessageId,
                ChannelId = entityMessage.ChannelId,
                ServerId = entityMessage.ServerId
            };
        }

        public static Entities.Member AsEntityType(this Models.Member modelMember)
        {
            return new Entities.Member
            {
                ClientUserId = modelMember.ClientUserId,
                NumPlayers = modelMember.NumPlayers
            };
        }

        public static Models.Member AsModelType(this Entities.Member entitiesMember)
        {
            return new Models.Member
            {
                ClientUserId = entitiesMember.ClientUserId,
                NumPlayers = entitiesMember.NumPlayers
            };
        }

        public static Entities.TeamoEntry AsEntityType(this Models.Post modelPost)
        {
            var members = modelPost.Members.ConvertAll((modelMember) => modelMember.AsEntityType());

            return new Entities.TeamoEntry
            {
                Id = modelPost.PostId,
                Message = modelPost.Message.AsEntityType(),
                Game = modelPost.Game,
                Members = members,
                EndDate = modelPost.EndDate,
                MaxPlayers = modelPost.MaxPlayers
            };
        } 

        public static Models.Post AsModelType(this Entities.TeamoEntry entityEntry)
        {
            var members = entityEntry.Members.ConvertAll((entityMember) => entityMember.AsModelType());

            return new Models.Post
            {
                PostId = entityEntry.Id.GetValueOrDefault(0),
                Message = entityEntry.Message.AsModelType(),
                Game = entityEntry.Game,
                Members = members,
                EndDate = entityEntry.EndDate,
                MaxPlayers = entityEntry.MaxPlayers
            };
        } 
    }
}
