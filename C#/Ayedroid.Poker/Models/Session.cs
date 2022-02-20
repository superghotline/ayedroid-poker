﻿namespace Ayedroid.Poker.Models
{
    public class Session : UniqueEntity
    {
        public Session(string sessionName) : base(sessionName)
        {
        }

        public List<Participant> Participants { get; } = new List<Participant>();
    }
}