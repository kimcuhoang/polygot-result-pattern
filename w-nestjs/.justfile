i:
    rm -rf node_modules
    yarn install

b:
    yarn build

s: b
    yarn start:dev