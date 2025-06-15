public struct TestResult
{
    public string testName;
    public float avgFps;
    public long avgBatches;
    public long avgTriangles;

    public override string ToString()
    {
        return $"--- {testName} ---\n" +
               $"FPS Medio: {avgFps:F1}\n" +
               $"Batches (SetPass Calls) Medios: {avgBatches}\n" +
               $"Triangulos Medios: {avgTriangles}\n";
    }
}

