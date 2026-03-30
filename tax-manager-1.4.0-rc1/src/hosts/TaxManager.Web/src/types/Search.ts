import { Taxation } from "./Taxation";
import { LegalEnvironment } from "./LegalEnvironment";

export interface Search {
  taxations: Taxation[];
  legalEnvironments: LegalEnvironment[];
}
