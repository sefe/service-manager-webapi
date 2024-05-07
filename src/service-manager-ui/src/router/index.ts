/**
 * Copyright 2024 SEFE Securing Energy for Europe GmbH
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

import type { Params } from "@vaadin/router";
import { Router } from "@vaadin/router";
import { routes } from "./routes";
import "./style-registrations";
export const router = new Router(document.querySelector("#outlet"));

router.setRoutes([
  // Redirect to URL without trailing slash
  {
    path: "(.*)/",
    action: (context, commands) => {
      const newPath = context.pathname.slice(0, -1);
      return commands.redirect(newPath);
    },
  },
  ...routes,
]);

export const urlForName = (name: string, params?: Params) =>
  router.urlForName(name, params);
