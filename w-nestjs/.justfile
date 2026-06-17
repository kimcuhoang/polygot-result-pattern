i:
    rm -rf node_modules
    yarn install

b:
    yarn build

s: b
    yarn start:dev

db-recreate:
    yarn db:drop
    yarn db:create

gen-migration name: b
    clear
    yarn typeorm:generate-migration src/infrastructures/typeorm/migrations/{{name}}

gen-seeding name:
    yarn typeorm-ext-seed:add src/infrastructures/typeorm/seedings/{{name}}

before-start: b
    yarn typeorm:run-migrations
    yarn typeorm-ext-seed:run