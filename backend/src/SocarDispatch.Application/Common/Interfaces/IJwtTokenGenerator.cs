using SocarDispatch.Domain.Entities;

namespace SocarDispatch.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}