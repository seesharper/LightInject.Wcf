namespace LightInject.Wcf.Tests
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Web.Caching;
    using System.Web.Hosting;
    using Xunit;

    [Collection("WcfTests")]
    public class VirtualPathProviderTests
    {                 
        [Fact]
        public void FileExists_NonExistingServiceFile_ReturnsTrue()
        {
            Assert.True(GetProvider().FileExists("~/NoSuchFile.svc"));                        
        }

        [Fact]
        public void FileExists_ExistingFile_ReturnsTrue()
        {            
            Assert.True(GetProvider().FileExists("~/ExistingFile"));                       
        }

        [Fact]
        public void FileExists_NonExistingFile_ReturnsFalse()
        {
            Assert.False(GetProvider().FileExists("~/NoSuchFile"));
        }

        [Fact]
        public void DirectoryExists_NonExistingServiceFile_ReturnsTrue()
        {
            Assert.True(GetProvider().DirectoryExists("~/SampleService.svc"));                        
        }

        [Fact]
        public void DirectoryExists_ExistingDirectory_ReturnsTrue()
        {
            Assert.True(GetProvider().DirectoryExists("~/ExistingDirectory"));
        }

        [Fact]
        public void DirectoryExists_NonExistingDirectory_ReturnsTrue()
        {
            Assert.False(GetProvider().DirectoryExists("~/NoSuchDirectory"));
        }

        [Fact]
        public void GetCacheDependency_ExistingServiceFile_ReturnsNull()
        {
            Assert.Null(GetProvider().GetCacheDependency("~/SampleService.svc",null, DateTime.MinValue));
        }

        [Fact]
        public void GetCacheDependency_NonServiceFile_CallsGetCacheDependencyInPrevious()
        {
            FileProvider.GetCacheDependencyCallCount = 0;
            GetProvider().GetCacheDependency("~/index.html", null, DateTime.MinValue);
            Assert.Equal(1, FileProvider.GetCacheDependencyCallCount);            
        }

        [Fact]
        public void GetFile_NonExistingServiceFile_ReturnsFile()
        {
            StreamReader streamReader = new StreamReader(GetProvider().GetFile("~/SampleService.svc").Open());

            var content = streamReader.ReadToEnd();

            Assert.Equal("<%@ ServiceHost Service=\"SampleService\" Factory = \"LightInject.Wcf.LightInjectServiceHostFactory, LightInject.Wcf\" %>", content);
        }

        //[Fact]
        //public void GetFile_ExistingServiceFile_ReturnsFileContent()
        //{
        //    StreamReader streamReader = new StreamReader(GetProvider().GetFile("~/NoSuchFile.svc").Open());
        //    var content = streamReader.ReadToEnd();

        //    Assert.Equal("SomeContent", content);
        //}


        private VirtualPathProvider GetProvider()
        {
            var previousField = typeof(VirtualPathProvider).GetField(
             "_previous",
             BindingFlags.Instance | BindingFlags.NonPublic);

            var provider = new VirtualSvcPathProvider();

            previousField.SetValue(provider, new FileProvider());

            return provider;
        }


        internal class FileProvider : VirtualSvcPathProvider
        {
            [ThreadStatic]
            public static int GetCacheDependencyCallCount;
            
            public override bool FileExists(string virtualPath)
            {                
                return virtualPath.EndsWith("ExistingFile");
            }

            public override bool DirectoryExists(string virtualDir)
            {
                return virtualDir.EndsWith("ExistingDirectory");
            }

            public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
            {
                GetCacheDependencyCallCount++;
                return null;
            }

            public override VirtualFile GetFile(string virtualPath)
            {
                return new VirtualSvcFile(virtualPath, "SomeContent");                                  
            }
        }
    }
}