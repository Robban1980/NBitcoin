namespace NBitcoin.Altcoins.GincoinInternals.Lyra2
{
	/// <summary>
	/// Adjusts the behaviour of the <see cref="Lyra2"/> algorithm.
	/// </summary>
	public enum Lyra2Version
	{
		/// <summary>
		/// Equivalent of 'LYRA2_old' function
		/// </summary>
		v1,

		/// <summary>
		/// Equivalent of 'LYRA2' function
		/// Fixes block absorption indices in setup phase.
		/// </summary>
		v2,

		/// <summary>
		/// Equivalent of 'LYRA2_3' function (Vertcoin)
		/// Wandering phase selects a random row.
		/// </summary>
		v3
	}
}
