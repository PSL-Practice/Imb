using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Imb.Data
{
    public interface ILibraryOperations
    {
        IEnumerable<BinaryStorageClass> Binaries { get; }
        BinaryStorageClass AddFile(byte[] data, string name, string[] path, DateTime fileDateUtc, string fileContainerPath);
        void DeleteFile(Guid id);
        void MoveFile(Guid id, string[] path, string name);
        void Rename(Guid id, string newName);
    }

    public interface ILibraryCreationOperations
    {
        ILibraryOperations Create(string path);
        ILibraryOperations Open(string path);
        void Close(ILibraryCreationOperations library);
    }
}