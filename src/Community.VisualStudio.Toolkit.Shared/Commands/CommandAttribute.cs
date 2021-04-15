using System;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Attribute for specifying command guids and ids.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Creates a new Command attribute instance.
        /// </summary>
        public CommandAttribute(string commandGuid, int commandId)
        {
            Guid = new Guid(commandGuid);
            Id = commandId;
        }
        
        /// <summary>
        /// The GUID of the command, often referred to as the Command Set Guid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The ID of the command, often expressed in hex.
        /// </summary>
        public int Id { get; set; }
    }
}
