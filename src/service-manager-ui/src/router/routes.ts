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

import type { Route } from "@vaadin/router";

export const routes: Route[] = [
  {
    path: "",
    component: "app-index",
    children: [
      {
        path: '/changes',
        name: 'changes',
        component: 'page-changes',
        action: async () => {
          await import('../pages/page-changes');
        }
      },
      {
        path: '',
        name: 'default',
        component: 'page-changes',
        action: async () => {
          await import('../pages/page-changes');
        }
      },
      {
        path: '(.*)',
        name: 'not-found',
        component: 'page-not-found',
        action: async () => {
          await import('../pages/page-not-found');
        }
      }
    ],
    action: async () => {
      await import("../components/app-index.ts");
    },
  },
];
