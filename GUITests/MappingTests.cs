using EDSEditorGUI2.Mapper;
using LibCanOpen;

namespace GUITests
{
    public class MappingTests
    {
        [Fact]
        public void MappingFromProtobuffer()
        {
            // testing for exception in the mapping assert.
            var sut = new CanOpenDevice();
            ProtobufferViewModelMapper.MapFromProtobuffer(sut);
        }
    }
}
