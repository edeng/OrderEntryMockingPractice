using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OrderEntryMockingPractice
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : this(new string[] {message})
        {
            
        }

        public ValidationException(IEnumerable<string> messages) : base(messages.First())
        {
            Messages = messages.ToList().AsReadOnly();
        }

        public ReadOnlyCollection<string> Messages { get; private set; }
    }
}