namespace M0LTE.Radio.Audio.Tests;

public class NullPttTests
{
    [Fact]
    public void NullPtt_is_an_IPttControl_whose_key_and_unkey_are_no_ops()
    {
        IPttControl ptt = new NullPtt();

        // The whole contract: keying and unkeying never throw and have no observable effect.
        Record.Exception(() =>
        {
            ptt.Key();
            ptt.Unkey();
        }).Should().BeNull();
    }
}
