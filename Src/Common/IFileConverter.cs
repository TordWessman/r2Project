using System;

namespace R2Core.Common
{
	public interface IFileConverter {
		
		void Convert(string inputFile, string outputFile);

		bool IsConverting { get; }

	}

}