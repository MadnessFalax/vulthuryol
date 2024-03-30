grammar PLC_Lab7_expr;

/** The start rule; begin parsing here. */
prog: (statement)+ ;

statement: ';'
    | TYPE ID (',' ID)* ';'
    | expr ';'
    | 'read' ID (',' ID)* ';'
    | 'write' expr (',' expr)* ';'
    | '{' statement (statement)* '}'
    | 'if' '(' condition ')' statement ('else' statement)?
    | 'while' '(' condition ')' statement
    ;

condition: expr
    ;

expr: unnary_expr
    | leaf operation?
    | assignment_expr
    | '(' expr ')'
    ;

unnary_expr: UN_MIN_OP expr
    | NEG_OP expr
    ;

assignment_expr: ID ASS_OP expr ;

operation: MUL_OP expr
    | ADD_OP expr
    | REL_OP expr
    | COMP_OP expr
    | AND_OP expr
    | OR_OP expr
    ;

leaf: ID
    | INT
    | BOOL
    | FLOAT
    | STRING
    ;

// Lexer

ASS_OP : '=' ;
UN_MIN_OP : '-' ;
NEG_OP : '!' ;
MUL_OP : '*' | '/' | '%' ;
ADD_OP : '+' | '-' | '.' ;
REL_OP : '<' | '>' ;
COMP_OP : '!=' | '==' ;
AND_OP : '&&' ;
OR_OP : '||' ;

TYPE : 'int'|'float'|'bool'|'string' ;
ID : [a-zA-Z][a-zA-Z0-9_]* ;        // match identifiers
INT : [1-9][0-9]*|'0' ;          // match integers
BOOL : 'true'|'false' ;          // match bool
FLOAT : [1-9][0-9]*'.'[0-9]* ;  // match float
STRING : '"'[a-zA-Z0-9_.+/*,'@&%=(!){[\]};<>: -]*'"' ; // match string
WS : [ \t\r\n]+ -> skip ;   // toss out whitespace