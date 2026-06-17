import 'dotenv/config';
import { DataSource } from "typeorm";
import { ConfigService } from '@nestjs/config';
import { getPostgresConnectionOptions } from './infrastructures/typeorm/datasource.properties';

// =================
// Kudos: https://stackoverflow.com/a/77328342
// =================
const configService: ConfigService = new ConfigService();

const dataSourceOptions = {
    ...getPostgresConnectionOptions(configService),
    seeds: [
        "dist/**/seedings/*.js"
    ],
    seedTracking: true
};

const dataSource = new DataSource(dataSourceOptions);

export default dataSource;

