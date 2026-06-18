
import { Injectable, Logger } from '@nestjs/common';
import * as appInsights from 'applicationinsights';
import { KnownSeverityLevel } from 'applicationinsights';
import pino from 'pino';

@Injectable()
export class ApplicationInsightStream {
    private readonly logger = new Logger(ApplicationInsightStream.name);
    
    public configApplicationInsightsStream(): pino.StreamEntry | null {

        const client = appInsights.defaultClient;
        if (!client) {
            this.logger.warn('Failed to initialize Application Insights client.');
            return null;
        }

        return {
            stream: {
                write: (streamChunk: string) => {
                    try {
                        const log = JSON.parse(streamChunk);
                        const { msg: message, level, err, ...customProperties } = log;

                        // 1. Fetch the active Application Insights telemetry context
                        const context = appInsights.getCorrelationContext();

                        // 2. Inject operational context IDs if they exist
                        const telemetryOptions: any = {
                            properties: customProperties,
                        };

                        if (context?.operation) {
                            telemetryOptions.tagOverrides = {
                                [client.context.keys.operationId]: context.operation.id,
                                [client.context.keys.operationParentId]: context.operation.parentId,
                                [client.context.keys.operationName]: context.operation.name,
                            };
                        }

                        // 3. Send correlated Exception Telemetry
                        if (err || level >= 50) {
                            client.trackException({
                                exception: err ? new Error(err.message) : new Error(message),
                                ...telemetryOptions,
                            });
                        }

                        // 4. Send correlated Trace Telemetry
                        client.trackTrace({
                            message: message || 'Structured log output',
                            severity: this.getSeverityLevel(level),
                            ...telemetryOptions,
                        });

                    } catch (error) {
                        this.logger.warn('Failed to forward log to Application Insights:', error);
                    }
                }
            }
        }
    }

    private getSeverityLevel(level: number): KnownSeverityLevel {
        if (level >= 60) return KnownSeverityLevel.Critical; // fatal
        if (level >= 50) return KnownSeverityLevel.Error;    // error
        if (level >= 40) return KnownSeverityLevel.Warning;  // warn
        if (level >= 30) return KnownSeverityLevel.Information; // info
        return KnownSeverityLevel.Verbose;                  // debug / trace
    }
}