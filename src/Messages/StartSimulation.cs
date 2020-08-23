using System;

namespace Messages
{
    public class StartSimulation
    {
        public int NumberOfAthletes { get; private set; }

        public StartSimulation(int numberOfAthletes)
        {
            NumberOfAthletes = numberOfAthletes;
        }
    }

    public class TestSimulation
    {
        public int NumberOfAthletes { get; private set; }

        public TestSimulation(int numberOfAthletes)
        {
            NumberOfAthletes = numberOfAthletes;
        }
    }
}
