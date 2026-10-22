public interface IPlatformFileIO
{
	string GetUserName();

	string GetPlatformName();

	string GetRootDataPath();

	bool Exists(string path);

	string ReadAllText(string path);

	byte[] ReadAllBytes(string path);

	void WriteAllText(string path, string contents);

	void WriteAllBytes(string path, byte[] bytes);

	void Delete(string path);
}
