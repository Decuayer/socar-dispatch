using MediatR;
using SocarDispatch.Application.Common.Models;
using SocarDispatch.Application.Features.Auth.DTOs;
using SocarDispatch.Domain.Enums;

namespace SocarDispatch.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(string? Search, string? Department, RoleType? RoleType) : IRequest<ApiResponse<List<UserDto>>>;
