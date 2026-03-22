// ── Add these to Presentation/DTOs/Dtos.cs ───────────────────────────────────
// Replace the existing CreateProfileRequest and UpdateProfileRequest records,
// and add the new DiscoveryProfileResponse and LocationUpdateRequest records.

// Replace CreateProfileRequest with:
using WebApp.Domain.Enums;
using WebApp.Presentation.DTOs;

public record CreateProfileRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Bio,
    string? Gender,
    string? City,
    string? Country);

// Replace UpdateProfileRequest with:
public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? Bio,
    string? Gender,
    string? City,
    string? Country);

// Replace ProfileResponse with:
public record ProfileResponse(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Bio,
    string? Gender,
    string? City,
    string? Country,
    double? Latitude,
    double? Longitude,
    AccountStatus Status,
    List<ProfileImageResponse> Images);

// Add this new record — used for discovery feed (includes distance, hides coordinates):
public record DiscoveryProfileResponse(
    Guid Id,
    string UserId,
    string FirstName,
    int Age,
    string? Bio,
    string? Gender,
    string? City,
    string? Country,
    double? DistanceKm,        // null if either user has no coordinates
    List<ProfileImageResponse> Images);

// Add this new record — called when user updates their location:
public record LocationUpdateRequest(
    string City,
    string? Country);

// Add this — returned after geocoding:
public record LocationUpdateResponse(
    string City,
    string Country,
    double Latitude,
    double Longitude);
