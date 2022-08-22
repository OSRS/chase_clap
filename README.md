# CHASE CLAP
CHASE Cyber Log Stream Analytic Processing (CLAP)

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](code_of_conduct.md)

CLAP is an open source code base resulting from the DARPA CHASE (Cyber Hunting at Scale) project. The initial CLAP development was funded by DARPA and developed by CCRi under a sub-contract to UVA as part of the P-CORE team.

## Initial Contributors:
-  frank.bentrem@ccri.com
-  michael.corsello@osrsopen.com
-  joshua.palm@ccri.com

## Funding: 
This work was funded by the Defense Advanced Research Projects Agency under contract no. W911NF-18-C-0019 through subcontract under the University of Virginia. Commonwealth Computer Research, Inc., Charlottesville, VA

# Introduction
The CLAP project is an initial reference implementation of a general capability for analyzing "cyber log like" data in either 1)real-time or 2)ad hoc (simulation mode).
The intent of the project is to provide a set of documented standards and conventions to structure a "CLAP compliant" implementation in any language or platform that could interoperate with any other "CLAP compliant" implementation. Specifically, to enable cross-organizational analytics based upon shared data with privacy as a key aspect of the design.

There should be a pathway for organizations to participate with minimal barrier to entry (in terms of data, hardware, software and labor costs).

## Goals
- Provide an extensible architecture for cyber log analytics
  - Support stream processing model for live and ad hoc scenarios
- Minimize resource requirements to ensure low bar to entry

- Focus on “global sharing” of data as a key differentiator
  - Feature sharing
  - Metric sharing (e.g., maliciousness/benignness “scores”)
  - Model (parameter) sharing
  - Standardization of data, models and processing
