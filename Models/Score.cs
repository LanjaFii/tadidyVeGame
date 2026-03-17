using System;

namespace TadidyVeGame.Models;

public record ScoreResponse(int Id, int PlayerId, int Value, DateTime CreatedAt);