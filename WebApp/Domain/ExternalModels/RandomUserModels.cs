namespace WebApp.Domain.ExternalModels;

// ── Raw API response models (maps directly to randomuser.me JSON) ─────────────

public record RandomUserApiResponse(
    List<RandomUserResult> Results,
    RandomUserInfo Info);

public record RandomUserInfo(
    string Seed,
    int Results,
    int Page,
    string Version);

public record RandomUserResult(
    string Gender,
    RandomUserName Name,
    RandomUserLocation Location,
    string Email,
    RandomUserLogin Login,
    RandomUserDob Dob,
    string Phone,
    RandomUserPicture Picture,
    string Nat);

public record RandomUserName(
    string Title,
    string First,
    string Last);

public record RandomUserLocation(
    RandomUserStreet Street,
    string City,
    string State,
    string Country,
    RandomUserCoordinates Coordinates,
    RandomUserTimezone Timezone);

public record RandomUserStreet(int Number, string Name);

public record RandomUserCoordinates(string Latitude, string Longitude);

public record RandomUserTimezone(string Offset, string Description);

public record RandomUserLogin(
    string Uuid,
    string Username,
    string Password,
    string Salt,
    string Md5,
    string Sha1,
    string Sha256);

public record RandomUserDob(string Date, int Age);

public record RandomUserPicture(
    string Large,
    string Medium,
    string Thumbnail);

// ── Cleaned model returned by IRandomUserService ──────────────────────────────

public record GeneratedUser(
    string FirstName,
    string LastName,
    string Email,
    string Gender,
    int Age,
    DateOnly DateOfBirth,
    string City,
    string Country,
    double Latitude,
    double Longitude,
    string PictureUrl,
    string PictureThumbnailUrl);
