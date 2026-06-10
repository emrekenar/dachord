namespace Infrastructure.Persistence;

using Amazon.DynamoDBv2.DataModel;

using Domain.Interfaces;
using Domain.Models.Feedback;
using Infrastructure.Entities;

public class FeedbackRepository(IDynamoDBContext dynamoDbContext) : IFeedbackRepository
{
    public async Task SaveAsync(Feedback feedback)
    {
        var item = new FeedbackItem
        {
            Pk = FeedbackEntity.PK,
            Sk = FeedbackEntity.SK(feedback.CreatedAt, feedback.Id),
            UserId = feedback.UserId,
            Email = feedback.Email,
            Message = feedback.Message,
            CreatedAt = feedback.CreatedAt,
        };
        await dynamoDbContext.SaveAsync(item);
    }
}
