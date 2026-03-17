using System;

namespace TadidyVeGame.Models;

public record PlayerResponse(int Id, string Username, string Bio, string ProfilePicture, int BestScore, DateTime CreatedAt);
public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password, string Bio, string ProfilePicture);
public record ScoreRequest(int PlayerId, int Value);