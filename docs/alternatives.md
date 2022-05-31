## Alternative packages

- [CorrelationId](https://www.nuget.org/packages/CorrelationId/) by Steve Gordon.
- [Correlate](https://www.nuget.org/packages/Correlate.AspNetCore/) by Martijn Bodeman.
- [AspNet.CorrelationIdGenerator](https://www.nuget.org/packages/AspNet.CorrelationIdGenerator) by Mark Gossa.

## Distributed tracing

- ["Improvements in .NET Core 3.0 for troubleshooting and monitoring distributed apps" by Sergey Kanzhelev (@SergeyKanzhelev)](https://devblogs.microsoft.com/aspnet/improvements-in-net-core-3-0-for-troubleshooting-and-monitoring-distributed-apps/)
- ["Building End-to-End Diagnostics and Tracing: Trace Context" by Jimmy Bogard (@jbogard)](https://jimmybogard.com/building-end-to-end-diagnostics-and-tracing-a-primer-trace-context/)

## Advanced tracing / further reading

Correlator is designed to solve simple scenario with reading and writing correlation ID from/to HTTP headers.
If you are looking for more advanced/distributed tracing, consider looking at:

- [OpenTelemetry](https://opentelemetry.io/)
- [OpenTracing](https://opentracing.io/)
- [Zipkin](https://zipkin.io/)
- [Jaeger](https://www.jaegertracing.io/)
