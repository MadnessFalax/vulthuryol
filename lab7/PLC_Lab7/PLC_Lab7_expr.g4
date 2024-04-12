grammar PLC_Lab7_expr;

/** The start rule; begin parsing here. */
prog: (statement)+ ;

statement: ';'                                                  # EmptyStatement
    | type_kw ID (',' ID)* ';'                                  # Declaration
    | expr ';'                                                  # Expression
    | 'read' ID (',' ID)* ';'                                   # ReadCLI
    | 'write' expr (',' expr)* ';'                              # WriteCLI
    | '{' statement (statement)* '}'                            # CodeBlock
    | 'if' '(' expr ')' statement ('else' statement)?           # IfStatement
    | 'while' '(' expr ')' statement                            # WhileStatement
    ;

expr: assignment_expr                                           # AssignmentLevel
    ;

unnary_expr: UN_MIN_OP expr                                     # UnaryMinus
    | NEG_OP expr                                               # UnaryNeg
    ;

assignment_expr: ID ASS_OP assignment_expr                      # NestedAss
    | assignment_leaf                                           # LeavingAss
    ;

assignment_leaf: leaf_common                                    # AssTerminal
    | or_expr                                                   # AssToOr
    ;

or_expr: or_expr OR_OP or_leaf                                  # NestedOr
    | or_leaf                                                   # LeavingOr
    ;

or_leaf: leaf_common                                            # OrTerminal
    | and_expr                                                  # OrToAnd
    ;

and_expr: and_expr AND_OP and_leaf                              # NestedAnd
    | and_leaf                                                  # LeavingAnd
    ;

and_leaf: leaf_common                                           # AndTerminal
    | comp_expr                                                 # AndToComp
    ;

comp_expr: comp_expr op=(EQ_OP|NOT_EQ_OP) comp_leaf             # NestedComp
    | comp_leaf                                                 # LeavingComp
    ;

comp_leaf: leaf_common                                          # CompTerminal
    | rel_expr                                                  # CompToRel
    ;

rel_expr: rel_expr op=(HIGH_OP|LOW_OP) rel_leaf                 # NestedRel
    | rel_leaf                                                  # LeavingRel
    ;

rel_leaf: leaf_common                                           # RelTerminal
    | add_expr                                                  # RelToAdd
    ;

add_expr: add_expr op=(ADD_OP|MIN_OP|CONCAT_OP) add_leaf        # NestedAdd
    | add_leaf                                                  # LeavingAdd
    ;

add_leaf: leaf_common                                           # AddTerminal
    | mul_expr                                                  # AddToMul
    ;

mul_expr: mul_expr op=(MUL_OP|DIV_OP|MOD_OP) mul_leaf           # NestedMul
    | mul_leaf                                                  # LeavingMul
    ;

mul_leaf : leaf_common                                          # MulTerminal
    ;          
    
leaf_common : leaf                                              # CommonLeaf
    | '(' expr ')'                                              # CommonParent
    | unnary_expr                                               # CommonUnnary
    ;

leaf: INT                               # Int
    | BOOL                              # Bool
    | FLOAT                             # Float
    | STRING                            # String
    | ID                                # Id
    ;

type_kw: type=INT_KW
    | type=FLOAT_KW
    | type=BOOL_KW
    | type=STRING_KW
    ;

// Lexer

ASS_OP : '=' ;
UN_MIN_OP : '-' ;
NEG_OP : '!' ;
MUL_OP : '*' ;
DIV_OP : '/' ;
MOD_OP : '%' ;
ADD_OP : '+' ;
MIN_OP : '-' ;
CONCAT_OP: '.' ;
LOW_OP : '<' ;
HIGH_OP : '>' ;
NOT_EQ_OP : '!=' ;
EQ_OP : '==' ;
AND_OP : '&&' ;
OR_OP : '||' ;

// types keywords
INT_KW: 'int' ;
FLOAT_KW: 'float' ;
BOOL_KW: 'bool' ;
STRING_KW: 'string' ;

ID : [a-zA-Z][a-zA-Z0-9_]* ;        // match identifiers
INT : [1-9][0-9]*|'0' ;          // match integers
BOOL : 'true'|'false' ;          // match bool
FLOAT : [1-9][0-9]*'.'[0-9]* ;  // match float
STRING : '"'[a-zA-Z0-9_.+/*,'@&%=(!){[\]};<>: -]*'"' ; // match string
WS : [ \t\r\n]+ -> skip ;   // toss out whitespace
COMMENT : '//'.*?'\n' -> skip;