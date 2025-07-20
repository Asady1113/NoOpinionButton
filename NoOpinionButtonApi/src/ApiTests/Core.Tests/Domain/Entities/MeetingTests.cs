using Core.Domain.Entities;

namespace Core.Tests.Domain.Entities;

public class MeetingTests
{
    [Fact]
    public void 正常系_VerifyPassword_司会者パスワード一致で正常認証()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = "meeting123",
            Name = "テスト会議",
            FacilitatorPassword = "admin123",
            ParticipantPassword = "user456"
        };
        var inputPassword = "admin123";

        // Act
        var result = meeting.VerifyPassword(inputPassword);

        // Assert
        Assert.Equal(PasswordType.Facilitator, result);
    }

    [Fact]
    public void 正常系_VerifyPassword_参加者パスワード一致で正常認証()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = "meeting123",
            Name = "テスト会議",
            FacilitatorPassword = "admin123",
            ParticipantPassword = "user456"
        };
        var inputPassword = "user456";

        // Act
        var result = meeting.VerifyPassword(inputPassword);

        // Assert
        Assert.Equal(PasswordType.Participant, result);
    }

    [Fact]
    public void 異常系_VerifyPassword_無効なパスワードで認証失敗()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = "meeting123",
            Name = "テスト会議",
            FacilitatorPassword = "admin123",
            ParticipantPassword = "user456"
        };
        var inputPassword = "wrongpassword";

        // Act
        var result = meeting.VerifyPassword(inputPassword);

        // Assert
        Assert.Equal(PasswordType.InvalidPassword, result);
    }

    [Fact]
    public void 異常系_VerifyPassword_大文字小文字が異なるパスワードで認証失敗()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = "meeting123",
            Name = "テスト会議",
            FacilitatorPassword = "admin123",
            ParticipantPassword = "user456"
        };
        var inputPassword = "Admin123";

        // Act
        var result = meeting.VerifyPassword(inputPassword);

        // Assert
        Assert.Equal(PasswordType.InvalidPassword, result);
    }

    [Fact]
    public void 異常系_VerifyPassword_スペースが含まれるパスワードで認証失敗()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = "meeting123",
            Name = "テスト会議",
            FacilitatorPassword = "admin123",
            ParticipantPassword = "user456"
        };
        var inputPassword = "admin123 ";

        // Act
        var result = meeting.VerifyPassword(inputPassword);

        // Assert
        Assert.Equal(PasswordType.InvalidPassword, result);
    }

    [Fact]
    public void 境界値_VerifyPassword_特殊文字を含むパスワードでの認証()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = "meeting123",
            Name = "テスト会議",
            FacilitatorPassword = "admin123",
            ParticipantPassword = "!@#$%^&*()_+-="
        };
        var inputPassword = "!@#$%^&*()_+-=";

        // Act
        var result = meeting.VerifyPassword(inputPassword);

        // Assert
        Assert.Equal(PasswordType.Participant, result);
    }

    [Fact]
    public void 境界値_VerifyPassword_マルチバイト文字を含むパスワードでの認証()
    {
        // Arrange
        var meeting = new Meeting
        {
            Id = "meeting123",
            Name = "テスト会議",
            FacilitatorPassword = "パスワード123",
            ParticipantPassword = "user456"
        };
        var inputPassword = "パスワード123";

        // Act
        var result = meeting.VerifyPassword(inputPassword);

        // Assert
        Assert.Equal(PasswordType.Facilitator, result);
    }
}