//PerformanceTester.cs
using UnityEngine;
using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using System;
using Unity.Profiling;
using UnityEditor;

public class PerformanceTester : MonoBehaviour
{
    [Header("Configuracoes do Teste")]
    [Tooltip("A camera que tera suas configuracoes alteradas")]
    [SerializeField] private Camera testCamera;

    [Header("Controle do Movimento")]
    [SerializeField] private Transform objectToMove;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private MonoBehaviour cameraMovementScript;

    [Header("Parametros das Configuracoes")]
    [SerializeField] private float baselineFarClip = 2000f;
    [SerializeField] private float optimizedFarClip = 150f;

    [Header("UI de Performance")]
    [SerializeField] private TextMeshProUGUI currentTestDisplay;
    [SerializeField] private TextMeshProUGUI fpsDisplay;
    [SerializeField] private TextMeshProUGUI batchesDisplay;
    [SerializeField] private TextMeshProUGUI trianglesDisplay;
    [SerializeField] private float uiUpdateInterval = 0.5f;
    [SerializeField] private float targetFps = 60f;

    [Header("Relatorio Final")]
    [SerializeField] private string reportFileName = "PerformanceReport.txt";

    private List<TestResult> results = new List<TestResult>();
    private bool goalReached = false;
    private ProfilerRecorder batchesRecorder;
    private ProfilerRecorder trianglesRecorder;

    void OnEnable()
    {
        batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        GoalTrigger.OnCameraReachedGoal += HandleGoalReached;
    }

    void OnDisable()
    {
        batchesRecorder.Dispose();
        trianglesRecorder.Dispose();
        GoalTrigger.OnCameraReachedGoal -= HandleGoalReached;
    }

    void Start()
    {
        if (testCamera == null || objectToMove == null || spawnPoint == null || cameraMovementScript == null)
        {
            Debug.LogError("Um ou mais campos (Camera, Object to Move, Spawn Point, ou Movement Script) nao foram definidos!");
            return;
        }

        cameraMovementScript.enabled = false;
        StartCoroutine(RunAllTests());
    }

    private void HandleGoalReached()
    {
        goalReached = true;
    }

    private IEnumerator RunAllTests()
    {
        Debug.Log("INICIANDO SUITE DE TESTES DE PERFORMANCE...");

        yield return StartCoroutine(RunTest("Configuracao A: Baseline", () =>
        {
            testCamera.farClipPlane = baselineFarClip;
            testCamera.useOcclusionCulling = false;
        }));

        yield return StartCoroutine(RunTest("Configuracao B: Frustum Culling", () =>
        {
            testCamera.farClipPlane = optimizedFarClip;
            testCamera.useOcclusionCulling = false;
        }));

        yield return StartCoroutine(RunTest("Configuracao C: Frustum + Occlusion", () =>
        {
            testCamera.farClipPlane = optimizedFarClip;
            testCamera.useOcclusionCulling = true;
        }));

        GenerateReport();
        currentTestDisplay.text = "Testes Concluidos!";
        QuitGame(3f);
    }

    private IEnumerator QuitGame(float time)
    { 
        yield return new WaitForSeconds(time);
        Application.Quit();
    }

    private IEnumerator RunTest(string testName, Action setupAction)
    {
        Debug.Log($"Iniciando: {testName}");
        currentTestDisplay.text = $"Preparando: {testName}...";
        cameraMovementScript.enabled = false;
        setupAction();
        goalReached = false;
        objectToMove.position = spawnPoint.position;
        objectToMove.rotation = spawnPoint.rotation;
        yield return null;

        currentTestDisplay.text = $"Rodando: {testName}...";
        cameraMovementScript.enabled = true;
        float totalFpsSum = 0;
        long totalBatchesSum = 0;
        long totalTrianglesSum = 0;
        int totalFrameCount = 0;
        float uiTimeAccumulator = 0;
        int uiFrameCount = 0;

        while (!goalReached)
        {
            totalFpsSum += 1f / Time.unscaledDeltaTime;
            totalBatchesSum += batchesRecorder.LastValue;
            totalTrianglesSum += trianglesRecorder.LastValue;
            totalFrameCount++;
            uiTimeAccumulator += Time.unscaledDeltaTime;
            uiFrameCount++;

            if (uiTimeAccumulator >= uiUpdateInterval)
            {
                UpdatePerformanceUI(uiFrameCount / uiTimeAccumulator);
                uiTimeAccumulator = 0f;
                uiFrameCount = 0;
            }
            yield return null;
        }

        cameraMovementScript.enabled = false;
        if (totalFrameCount == 0) totalFrameCount = 1;
        TestResult result = new TestResult
        {
            testName = testName,
            avgFps = totalFpsSum / totalFrameCount,
            avgBatches = totalBatchesSum / totalFrameCount,
            avgTriangles = totalTrianglesSum / totalFrameCount
        };
        results.Add(result);

        Debug.Log($"Finalizado: {testName} | FPS Medio: {result.avgFps:F1}");
    }

    void UpdatePerformanceUI(float averageFps)
    {
        if (fpsDisplay == null) return;
        fpsDisplay.text = $"FPS: {averageFps:F0}";
        batchesDisplay.text = $"Batches: {batchesRecorder.LastValue}";
        trianglesDisplay.text = $"Triangulos: {trianglesRecorder.LastValue}";
        if (averageFps >= targetFps * 0.9f) { fpsDisplay.color = Color.green; }
        else if (averageFps < targetFps * 0.4f) { fpsDisplay.color = Color.red; }
        else { fpsDisplay.color = Color.yellow; }
    }

    private void GenerateReport()
    {
        StringBuilder reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("RELATORIO DE PERFORMANCE");
        reportBuilder.AppendLine($"Data: {System.DateTime.Now}");
        reportBuilder.AppendLine("========================================");
        foreach (var result in results) { reportBuilder.AppendLine(result.ToString()); reportBuilder.AppendLine(); }
        reportBuilder.AppendLine("========================================");
        reportBuilder.AppendLine("Fim do Relatorio.");

        string projectRootPath = Path.GetDirectoryName(Application.dataPath);

        string directoryPath = Path.Combine(projectRootPath, "Resultados");

        Directory.CreateDirectory(directoryPath);

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string uniqueReportFileName = $"{Path.GetFileNameWithoutExtension(reportFileName)}_{timestamp}{Path.GetExtension(reportFileName)}";

        string filePath = Path.Combine(directoryPath, uniqueReportFileName);

        File.WriteAllText(filePath, reportBuilder.ToString());
        Debug.Log($"SUITE DE TESTES CONCLUIDA! Relatorio salvo em: {filePath}");
    }
}