import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
  input: './openapi/database_api_service.json',       
  output: 'src/api/database',                         
  plugins: ['@hey-api/client-axios']                  
});