using System;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Attribute for specifying command guids and ids.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Registers the command for handling the command ID and assumes the package GUID is used as command GUID.
        /// </summary>
        public CommandAttribute(int commandId)
         : this("00000000-0000-0000-0000-000000000000", commandId) { }

        /// <summary>
        /// Registers the command for handling the command with the specified GUID and ID.
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
