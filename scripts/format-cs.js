const { execSync } = require('child_process');
const path = require('path');

const root = path.resolve(__dirname, '..');
const projects = [
  path.join(root, 'src', 'Transacciones.Core', 'Transacciones.Core.csproj'),
  path.join(root, 'src', 'Transacciones.API', 'Transacciones.API.csproj'),
];

for (const project of projects) {
  execSync(`dotnet format  "${project}"`, { // --verify-no-changes para no modificar los archivos solo probarr
    stdio: 'inherit',
    cwd: root,
  });
}
