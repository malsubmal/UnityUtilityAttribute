// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using UtilAttribute;

namespace SampleAnalyzer.Sample
{
// If you don't see warnings, build the Analyzers Project.

    public class Examples
    {
        public class MyCompanyClass // Try to apply quick fix using the IDE.
        {
        }

        public void ToStars()
        {
            var spaceship = new Spaceship();
            spaceship.SetSpeed(300000000); // Invalid value, it should be highlighted.
            spaceship.SetSpeed(42);
        }
        
        [MustCallBase]
        public virtual void SampleMethod()
        {}

        public void Hello(int i)
        {
            
        }
    }
    
    public class Child : Examples
    {
        public override void SampleMethod()
        {
            base.SampleMethod();
            var i = 1;
            i++;
        }
    }

    public class SubChild : Child
    {
        public override void SampleMethod()
        {
            base.SampleMethod();
        }
    }
}