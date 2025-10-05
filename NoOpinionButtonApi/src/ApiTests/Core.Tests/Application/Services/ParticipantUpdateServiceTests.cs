using Core.Application;
using Core.Application.Ports;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Domain.Ports;
using Core.Domain.ValueObjects.Participant;
using Core.Domain.ValueObjects.Meeting;
using Moq;

namespace Core.Tests.Application.Services;

public class ParticipantUpdateServiceTests
{
    private readonly Mock<IParticipantRepository> _participantRepositoryMock;
    private readonly IParticipantUpdateService _participantUpdateService;

    public ParticipantUpdateServiceTests()
    {
        _participantRepositoryMock = new Mock<IParticipantRepository>();
        // NOTE: ParticipantUpdateService is not implemented yet - this will fail
        _participantUpdateService = new ParticipantUpdateService(_participantRepositoryMock.Object);
    }

    [Fact]
    public async Task 正常系_UpdateParticipantNameAsync_有効な入力で名前更新成功()
    {
        // Arrange
        var participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
        {
            ParticipantId = "participant123",
            Name =  "新しい名前"
        };
        
        var existingParticipant = new Participant(
            participantUpdateServiceRequest.ParticipantId,
            "古い名前", 
            "meeting123",
            2,
            true,
            true
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<ParticipantId>()))
            .ReturnsAsync(existingParticipant);
        _participantRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Participant>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(participantUpdateServiceRequest.Name, result.UpdatedName);
        
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ParticipantId>()), Times.Once);
        _participantRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Participant>()), Times.Once);
    }

    [Fact]
    public async Task 異常系_UpdateParticipantNameAsync_参加者IDがnullの場合ArgumentException()
    {
        // Arrange
        var participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
        {
            ParticipantId = null,
            Name =  "新しい名前"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest));
        
        Assert.Contains("参加者IDは空にできません", exception.Message);
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ParticipantId>()), Times.Never);
    }

    [Fact] 
    public async Task 異常系_UpdateParticipantNameAsync_参加者IDが空文字の場合ArgumentException()
    {
        // Arrange
        var participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
        {
            ParticipantId = "",
            Name =  "新しい名前"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest));
        
        Assert.Contains("参加者IDは空にできません", exception.Message);
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ParticipantId>()), Times.Never);
    }

    [Fact]
    public async Task 異常系_UpdateParticipantNameAsync_新しい名前がnullの場合ArgumentException()
    {
        // Arrange
        var participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
        {
            ParticipantId = "participant123",
            Name =  null
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest));
        
        Assert.Contains("参加者名は空にできません", exception.Message);
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ParticipantId>()), Times.Never);
    }

    [Fact]
    public async Task 異常系_UpdateParticipantNameAsync_新しい名前が空文字の場合ArgumentException()
    {
        // Arrange 
        var participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
        {
            ParticipantId = "participant123",
            Name =  ""
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest));
        
        Assert.Contains("参加者名は空にできません", exception.Message);
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ParticipantId>()), Times.Never);
    }

    [Fact]
    public async Task 異常系_UpdateParticipantNameAsync_新しい名前が50文字超の場合ArgumentException()
    {
        // Arrange
        var participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
        {
            ParticipantId = "participant123",
            Name =  new string('あ', 51) // 51文字
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest));
        
        Assert.Contains("参加者名は50文字以内である必要があります", exception.Message);
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ParticipantId>()), Times.Never);
    }

    [Fact]
    public async Task 異常系_UpdateParticipantNameAsync_参加者が存在しない場合KeyNotFoundException()
    {
        // Arrange
        var participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
        {
            ParticipantId = "participant123",
            Name =  "新しい名前"
        };

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<ParticipantId>()))
            .ReturnsAsync((Participant)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest));
        
        Assert.Contains("参加者", exception.Message);
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ParticipantId>()), Times.Once);
        _participantRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Participant>()), Times.Never);
    }

    [Fact]
    public async Task 異常系_UpdateParticipantNameAsync_参加者が非アクティブの場合InvalidOperationException()
    {
        // Arrange
        var participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
        {
            ParticipantId = "participant123",
            Name =  "新しい名前"
        };
        
        var inactiveParticipant = new Participant(
            participantUpdateServiceRequest.ParticipantId,
            "古い名前",
            "meeting123", 
            2,
            true,
            false  // IsActive = false
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<ParticipantId>()))
            .ReturnsAsync(inactiveParticipant);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest));
        
        Assert.Contains("非アクティブ", exception.Message);
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ParticipantId>()), Times.Once);
        _participantRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Participant>()), Times.Never);
    }

    [Fact]
    public async Task 正常系_UpdateParticipantNameAsync_リポジトリメソッドが適切に呼ばれること()
    {
        // Arrange
       var participantUpdateServiceRequest = new ParticipantUpdateServiceRequest
       {
           ParticipantId = "participant123",
           Name =  "新しい名前"
       };
        
        var existingParticipant = new Participant(
            participantUpdateServiceRequest.ParticipantId,
            "古い名前",
            "meeting123",
            2,
            true,
            true
        );

        _participantRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<ParticipantId>()))
            .ReturnsAsync(existingParticipant);
        _participantRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Participant>()))
            .Returns(Task.CompletedTask);

        // Act
        await _participantUpdateService.UpdateParticipantNameAsync(participantUpdateServiceRequest);

        // Assert - Repository calls verification
        _participantRepositoryMock.Verify(x => x.GetByIdAsync(
            It.Is<ParticipantId>(id => id.Value == participantUpdateServiceRequest.ParticipantId)), Times.Once);
        _participantRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<Participant>(p => p.Name.Value == participantUpdateServiceRequest.Name)), Times.Once);
    }
}