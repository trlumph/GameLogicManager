using HazelcastBasics;


namespace HazelcastTests;

public class HazelcastTests
{
    public class ShouldReturnExpectedNumber
    {
        [Fact]
        public void PessimisticLocks()
        {
            // Arrange
            var map = HazelcastMapping.MapFactory().Result;
            var expected = 30000;
            
            // Act
            var result = HazelcastMapping.PessimisticLocks(map).Result;
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void OptimisticLocks()
        {
            // Arrange
            var map = HazelcastMapping.MapFactory().Result;
            var expected = 30000;
            
            // Act
            var result = HazelcastMapping.OptimisticLocks(map).Result;
            
            // Assert
            Assert.Equal(expected, result);
        }
    }
}