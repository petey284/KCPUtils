using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace KCPUtils.TestHelpers
{
    public class MockedFileSystem
    {
        public class Builder
        {
            private string currentDirectory;
            private readonly Dictionary<string, MockFileData> fileContents = new Dictionary<string, MockFileData>();

            public Builder() { }
            public Builder(string currentDirectory) { this.currentDirectory = currentDirectory; }

            public Builder AddFile(string filename, MockFileData contents)
            {
                this.fileContents.Add(filename, contents);
                return this;
            }

            public Builder CurrentDirectory(string currentDirectory)
            {
                if (this.currentDirectory != null) { return this; }

                this.currentDirectory = currentDirectory;
                return this;
            }

            public IFileSystem Build()
            {
                return new MockFileSystem(this.fileContents, this.currentDirectory);
            }
        }
    }
}