using Demon.Slice.Domain;
using NUnit.Framework;
namespace Demon.Slice.Tests
{
    public sealed class FinalTabletProgressTests
    {
        [Test] public void PlacementRequiresAcquisition()
        { var state = new FinalTabletProgress(); Assert.That(state.Place(), Is.False); Assert.That(state.Placed, Is.False); Assert.That(state.Acquired, Is.False); }
        [Test] public void CollectionDoesNotImplicitlyPlaceAndCannotRepeat()
        { var state = new FinalTabletProgress(); Assert.That(state.Collect(), Is.True); Assert.That(state.Placed, Is.False); Assert.That(state.Collect(), Is.False); }
        [Test] public void PlacementIsTerminalAndPreservesAcquisition()
        { var state = new FinalTabletProgress(); state.Collect(); Assert.That(state.Place(), Is.True); Assert.That(state.Place(), Is.False); Assert.That(state.Collect(), Is.False); Assert.That(state.Acquired && state.Placed, Is.True); }
    }
}
