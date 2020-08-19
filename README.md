# Net
Net is a circuit simulator based on my previous work (Node). Node can simulate simple circuit using netlist input. It was written in C++. Net is built on same principal but using different system. Net uses schematics to take a network input. Simulator engine then loops throgh each component and makes a system of equations. Each component model has it's own behavior for the equation matrix. They update the matrix according to their properties and keep track of how they can find their post-simulation properties.
Proper description of each section of the code will be added soon inshallah.
