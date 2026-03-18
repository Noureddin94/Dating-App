using WebApp.Domain.Enums;

namespace WebApp.Presentation.DTOs;

// ── Profile ───────────────────────────────────────────────────────────────────

public class UploadImageRequest
{
    public IFormFile File { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

public record CreateProfileRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Bio,
    string? Gender,
    string? Location);

public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? Bio,
    string? Gender,
    string? Location);

public record ProfileResponse(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Bio,
    string? Gender,
    string? Location,
    AccountStatus Status,
    List<ProfileImageResponse> Images);

public record ProfileImageResponse(
    Guid Id,
    string BlobPath,
    bool IsPrimary,
    int SortOrder);

// ── Like ──────────────────────────────────────────────────────────────────────

public record SendLikeRequest(string ReceiverId, bool IsLike);

public record LikeResponse(
    Guid Id,
    string SenderId,
    string ReceiverId,
    bool IsLike,
    DateTime CreatedAt,
    bool MatchCreated);

public record DailyLikeStatusResponse(int Used, int Limit, int Remaining);

// ── Match ─────────────────────────────────────────────────────────────────────

public record MatchResponse(
    Guid Id,
    string OtherUserId,
    string OtherUserFirstName,
    string? OtherUserPrimaryImage,
    Guid ConversationId,
    DateTime MatchedAt);

// ── Message ───────────────────────────────────────────────────────────────────

public record SendMessageRequest(string Content);

public record SendMessageToUserRequest(string ReceiverId, string Content);

public record MessageResponse(
    Guid Id,
    string SenderId,
    string Content,
    DateTime SentAt,
    bool IsRead);

public record UnmatchedMessageStatusResponse(int Used, int Limit, int Remaining);

// ── Game ──────────────────────────────────────────────────────────────────────

public record SendGameInviteRequest(string ReceiverId, string GameType);

public record GameInviteResponse(
    Guid Id,
    string SenderId,
    string SenderFirstName,
    string ReceiverId,
    string GameType,
    InviteStatus Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);

public record GameSessionResponse(
    Guid Id,
    string GameType,
    string Player1Id,
    string Player2Id,
    string StateJson,
    SessionStatus Status,
    DateTime StartedAt);

public record UpdateGameStateRequest(string StateJson);

// ── Moderation ────────────────────────────────────────────────────────────────

public record ReportUserRequest(string ReportedUserId, string Reason);

// ── Admin ─────────────────────────────────────────────────────────────────────

public record AdminUserResponse(
    string UserId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    AccountStatus Status,
    string? Location,
    int ImageCount);