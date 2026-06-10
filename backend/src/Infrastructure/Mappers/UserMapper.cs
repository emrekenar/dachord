namespace Infrastructure.Mappers;

using Domain.Models.User;
using Infrastructure.Entities;

public class UserMapper
{
    public static User MapToDomainModel(UserItem entity)
    {
        return new User
        {
            Id = entity.Id,
            Email = entity.Email!,
            PasswordHash = entity.PasswordHash!,
            DisplayName = entity.DisplayName ?? string.Empty,
            Bio = entity.Bio ?? string.Empty,
            AvatarIcon = entity.AvatarIcon ?? string.Empty,
            Role = MapToDomainRole(entity.Role),
            NumberOfApprovedSongs = entity.NumberOfApprovedSongs,
            NumberOfLikes = entity.NumberOfLikes,
        };
    }

    public static UserItem MapToEntity(User user)
    {
        return new UserItem
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            AvatarIcon = user.AvatarIcon,
            Role = MapToEntityRole(user.Role),
            NumberOfApprovedSongs = user.NumberOfApprovedSongs,
            NumberOfLikes = user.NumberOfLikes,
        };
    }

    public static UserRoleEnum MapToEntityRole(UserRole role)
    {
        return role switch
        {
            UserRole.User => UserRoleEnum.User,
            UserRole.Moderator => UserRoleEnum.Moderator,
            UserRole.Admin => UserRoleEnum.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(role), $"Unexpected role value: {role}"),
        };
    }

    public static UserRole MapToDomainRole(UserRoleEnum roleEnum)
    {
        return roleEnum switch
        {
            UserRoleEnum.User => UserRole.User,
            UserRoleEnum.Moderator => UserRole.Moderator,
            UserRoleEnum.Admin => UserRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(roleEnum), $"Unexpected role enum value: {roleEnum}"),
        };
    }
}