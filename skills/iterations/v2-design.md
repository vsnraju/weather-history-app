
## Future Improvements

If this solution were scaled for larger datasets or higher throughput, I would consider:

- Introducing bounded parallel processing using `Parallel.ForEachAsync`
- Limiting concurrency with `MaxDegreeOfParallelism`
- Adding rate-limit handling for external API calls
- Using background workers or queues for batch processing
- Adding distributed caching (Redis)
- Moving storage to cloud object storage
- Adding observability with OpenTelemetry

For this exercise, I intentionally kept processing mostly sequential because:
- input size is very small
- sequential processing is easier to debug
- deterministic execution improves readability
- avoiding premature optimization keeps the solution simpler
```
