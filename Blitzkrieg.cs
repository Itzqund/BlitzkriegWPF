using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BlitzkriegWPF
{
    class Blitzkrieg
    {
        string[] 
            runs,
            notes;
        bool[] checks;
        int[] attempts;
        
        public Blitzkrieg(string[] sendedRuns, bool[] sendedChecks, int[] sendedAttempts, string[] sendedNotes)
        {
            runs = sendedRuns;
            checks = sendedChecks;
            attempts = sendedAttempts;
            notes = sendedNotes;
        }
    }
}
